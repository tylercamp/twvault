
//  TODO - Remove duplicate logic by using RateLimiter internally

function RequestManager() {
    this.pendingRequests = [];
    this.errorHistory = {};
    this.maxRequestsPerSecond = 5;
    this.refreshDelay = 100;

    this._urlHistory = {};

    this._interval = null;
    this._onFinishedHandler = null;
    this._hasErrors = false;
    this._requestTimes = [];

    this.resetStats();
}

RequestManager.prototype.start = function () {
    if (this._interval) {
        return;
    }

    if (!this.pendingRequests.length) {
        this._onFinishedHandler && this._onFinishedHandler();
        return;
    }

    var self = this;
    var numResponding = 0;

    this._interval = setInterval(() => {
        var now = new Date();
        for (var i = 0; i < self._requestTimes.length; i++) {
            if (now.valueOf() - self._requestTimes[i].valueOf() >= 1000) {
                self._requestTimes.splice(i, 1);
                --i;
            }
        }

        if (self._requestTimes.length < self.maxRequestsPerSecond && self.pendingRequests.length > 0) {
            (() => {
                var request = self.pendingRequests[0];
                self.pendingRequests.splice(0, 1);

                if (request.numErrors >= 3) {
                    self.stats.done++;
                    return;
                }

                if (request.beforeRun && !request.beforeRun(request)) {
                    if (!self.pendingRequests.length && numResponding == 0) {
                        self._onFinishedHandler && self._onFinishedHandler();
                    }
                    self.stats.done++;
                    return;
                }

                self._requestTimes.push(new Date());

                //console.log('Getting ', request);
                numResponding++;

                $.ajax(request)
                    .done((data, result) => {
                        self.stats.done++;
                        var numCompleted = self.stats.done + "/" + self.stats.total;
                        //console.log(numCompleted);
                        numResponding--;

                        var err;
                        try {
                            request.onDone && request.onDone(data, request);
                        } catch (e) {
                            self.stats.numFailed++;
                            err = e;
                        }

                        if (!self.pendingRequests.length && numResponding == 0) {
                            self._onFinishedHandler && self._onFinishedHandler();
                        }

                        if (err) {
                            throw err;
                        }
                    })
                    .fail(() => {
                        console.log('error, requeueing', arguments);
                        request.numErrors++;
                        if (request.numErrors < 3) {
                            self.pendingRequests.push(request);
                            numResponding--;
                        } else {
                            console.log('request failed too many times, stopping requests for ', request);
                            numResponding--;
                            self._hasErrors = true;
                            self.stats.numErrors++;

                            request.onDone && request.onDone(null, request);

                            if (!self.pendingRequests.length && numResponding == 0) {
                                self._onFinishedHandler && self._onFinishedHandler();
                            }
                        }
                    });
            })();
        }
    }, this.refreshDelay);
};

RequestManager.prototype.stop = function () {
    this._interval && clearInterval(this._interval);
    this._interval = null;
};

RequestManager.prototype.isRunning = function () {
    return this._interval != null;
}

RequestManager.prototype.addRequest = function (url, callback, beforeRunCallback_) {
    if (this._urlHistory[url]) {
        console.log('Duplicate URL request!');
        debugger;
    } else {
        this._urlHistory[url] = true;
    }

    this.stats.total++;
    this.pendingRequests.push({
        url: url,
        onDone: callback,
        numErrors: 0,
        beforeRun: beforeRunCallback_
    });
};

RequestManager.prototype.setFinishedHandler = function (callback) {
    this._onFinishedHandler = callback;
};

RequestManager.prototype.getStats = function () {
    return this.stats;
};

RequestManager.prototype.hasRequests = function () {
    return this.pendingRequests.length > 0;
};

RequestManager.prototype.hasErrors = function () {
    return this._hasErrors;
};

RequestManager.prototype.resetStats = function () {
    this.stats = new RequestManagerStats();
};


function RequestManagerStats() {
    this.done = 0;
    this.pending = 0;
    this.total = 0;
    this.numFailed = 0;
}

RequestManagerStats.prototype.toString = function () {
    return lib.translate(lib.itlcodes.REQUEST_STATS, { numDone: this.done, numTotal: this.total, numFailed: this.numFailed });
};
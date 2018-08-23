

function RequestManager() {
    this.pendingRequests = [];
    this.stats = {
        done: 0,
        pending: 0,
        total: 0,
        numFailed: 0
    }
    this.errorHistory = {};
    this.maxPendingRequests = 2;
    this.refreshDelay = 500;

    this._urlHistory = {};

    this._interval = null;
    this._onFinishedHandler = null;
    this._hasErrors = false;
}

RequestManager.prototype.start = function () {
    if (this._interval) {
        return;
    }

    var self = this;
    var numResponding = 0;

    this._interval = setInterval(() => {
        while (numResponding < self.maxPendingRequests && self.pendingRequests.length > 0) {
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

                console.log('Getting ', request);
                numResponding++;

                $.ajax(request)
                    .done((data, result) => {
                        self.stats.done++;
                        var numCompleted = self.stats.done + "/" + self.stats.total;
                        console.log(numCompleted);
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

RequestManager.prototype.addManyRequests = function (urls, urlCallback, allCallback, beforeUrlCallback_) {
    var numCompleted = 0;
    function track(data, request) {
        urlCallback && urlCallback(data, request);
        if (++numCompleted == urls.length) {
            allCallback && allCallback();
        }
    }

    var self = this;
    urls.forEach((url) => {
        self.addRequest(url, track, beforeUrlCallback_);
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
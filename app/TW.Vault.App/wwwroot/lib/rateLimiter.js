

function RateLimiter() {
    this.pendingTasks = [];
    this.maxTasksPerSecond = 5;
    this.refreshDelay = 100;

    this._interval = null;
    this._onFinishedHandler = null;
    this._taskTimes = [];

    this.resetStats();
}

RateLimiter.prototype.start = function () {
    if (this._interval) {
        return;
    }

    var self = this;
    var numWorking = 0;

    this._interval = setInterval(() => {
        var now = new Date();
        for (var i = 0; i < self._taskTimes.length; i++) {
            if (now.valueOf() - self._taskTimes[i].valueOf() >= 1000) {
                self._taskTimes.splice(i, 1);
                --i;
            }
        }

        if (self._taskTimes.length < self.maxTasksPerSecond && self.pendingTasks.length > 0) {
            (() => {
                var task = self.pendingTasks[0];
                self.pendingTasks.splice(0, 1);
                self._taskTimes.push(new Date());

                numWorking++;

                let doneIndicator = task();
                if (!doneIndicator) {
                    self.stats.done++;
                    numWorking--;

                    if (!self.pendingTasks.length && numWorking == 0) {
                        self._onFinishedHandler && self._onFinishedHandler();
                    }
                } else {
                    doneIndicator(() => {
                        numWorking--;

                        if (!self.pendingTasks.length && numWorking == 0) {
                            self._onFinishedHandler && self._onFinishedHandler();
                        }
                    });
                }
            })();
        }
    }, this.refreshDelay);
};

RateLimiter.prototype.stop = function () {
    this._interval && clearInterval(this._interval);
    this._interval = null;
};

RateLimiter.prototype.isRunning = function () {
    return this._interval != null;
}

RateLimiter.prototype.addTask = function (task) {
    this.stats.total++;
    this.pendingTasks.push(task);
};

RateLimiter.prototype.setFinishedHandler = function (callback) {
    this._onFinishedHandler = callback;
};

RateLimiter.prototype.getStats = function () {
    return this.stats;
};

RateLimiter.prototype.hasTasks = function () {
    return this.pendingTasks.length > 0;
};

RateLimiter.prototype.resetStats = function () {
    this.pendingTasks = [];
    this._taskTimes = [];
    this.stats = {
        done: 0,
        pending: 0,
        total: 0,
        numFailed: 0
    };
};
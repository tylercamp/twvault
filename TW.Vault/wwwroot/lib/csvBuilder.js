function CsvBuilder() {
    this.rows = [];
}

CsvBuilder.prototype.addRow = function addRow(...data) {
    this.rows.push(data);
};

CsvBuilder.prototype.addBlank = function addBlank(numLines_) {
    numLines_ = numLines_ || 1;
    for (var i = 0; i < numLines_; i++)
        this.rows.push([]);
};

CsvBuilder.prototype.makeCsvString = function makeCsvString() {
    var numColumns = 0;
    this.rows.forEach((r) => numColumns = Math.max(numColumns, r.length));

    // Specify the column separator explicitly so excel isn't dumb
    var csvRows = ['sep=,'];
    this.rows.forEach((row) => {
        row = row.slice();
        for (var i = 0; i < row.length - numColumns; i++)
            row.push('');
        csvRows.push(row.join(','));
    });
    return csvRows.join('\n');
};
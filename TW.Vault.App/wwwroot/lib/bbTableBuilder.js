function BBTableBuilder(tableCode) {
    if (!tableCode)
        throw "Param tableCode not provided!";

    this._tableCode = tableCode;
    this._header = [];
    this._rows = [];
}

BBTableBuilder.prototype.setColumnNames = function setColumnNames(...names) {
    this._header = names;
}

BBTableBuilder.prototype.addRow = function addRow(...data) {
    this._rows.push(data);
};

BBTableBuilder.prototype.toString = function toString() {
    let headerRow = `[**]${this._header.join('[||]')}[/**]`;
    let tableContents = this._rows.map(r => `[*]${r.join('[|]')}`).join('\n');
    
    return `[${this._tableCode}]\n${headerRow}\n${tableContents}\n[/${this._tableCode}]`;
};
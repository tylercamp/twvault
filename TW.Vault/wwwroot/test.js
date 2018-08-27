//  Random file for testing different things

(() => {
    //# REQUIRE lib.js

    var groups = lib.detectGroups();
    var pages = lib.detectMultiPages();

    console.log('Groups: ', groups);
    console.log('Pages: ', pages);

})();
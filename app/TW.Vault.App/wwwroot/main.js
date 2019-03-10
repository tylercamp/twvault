

try {

    if (5 != eval("(() => 5)()")) {
        alertInvalidBrowser();
    } else if ("hello" != eval("`hello`")) {
        alertInvalidBrowser();
    } else if (5 != eval("let x = 5; x")) {
        alertInvalidBrowser();
    } else if (!window.MutationObserver) {
        alertInvalidBrowser();
    } else {
        $.ajax({
            url: getScriptBase() + "/vault.js?_=" + Math.round(Math.random() * 1000000),
            dataType: 'script'
        });
    }

} catch (e) {
    alertInvalidBrowser();
}



function alertInvalidBrowser() {
    alert("Your browser isn't capable of running the Vault, please try again using a recent version of Chrome, Firefox, Safari, or Edge.");
}

function getScriptBase() {
    var ex = new Error();
    var stack = ex.stack.split('\n');
    for (var i = 0; i < stack.length; i++)
        stack[i] = stack[i].trim();
    var firstScope = stack[stack.length - 1];
    var sourceUrl = firstScope.match(/(https?:\/\/.+\.js)/);
    if (sourceUrl)
        sourceUrl = sourceUrl[1];

    sourceUrl = sourceUrl || 'https://v.tylercamp.me/script/main.js';

    var parts = sourceUrl.split('/');
    var result = [];
    for (var i = 0; i < parts.length - 1; i++) {
        result.push(parts[i]);
    }
    return result.join('/');
}
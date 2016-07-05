var page = require('webpage').create(),
    sytem = require('system');

page.onLoadFinished = function () {
    console.log(page.content);
    phantom.exit();
}

page.open(system.args[1]);
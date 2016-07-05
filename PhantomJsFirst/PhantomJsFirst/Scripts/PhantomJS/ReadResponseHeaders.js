var system = require('system');
var page = require('webpage').create();

page.onResourceReceived = function (response) {
    console.log('Response (#' + response.id + ', stage "' + response.stage + '"): ' + JSON.stringify(response));
};

page.open(system.args[1]);
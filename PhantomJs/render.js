var page = require('webpage').create(),
    system = require('system'),
    t, address, err = '';

function errlog(msg) {
    err += msg + '\n';
}

if (system.args.length === 1) {
    console.log('Usage: loadspeed.js <some URL>');
    phantom.exit(1);
} else {
    t = Date.now();
    address = system.args[1];

    phantom.outputEncoding = "utf8";
    //page.settings.userAgent = 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.120 Safari/537.36';
    page.settings.loadImages = false;
    //page.settings.webSecurityEnabled = false;
    //page.settings.resourceTimeout = 3000;
    //page.viewportSize = { width: 1920, height: 1080 };

    //setup resource handling
    page.onResourceRequested = function (request) {
        errlog('Request ' + JSON.stringify(request, undefined, 4));
    };

    //set up error handling
    page.onError = function (msg, trace) {
        errlog('Error: ' + msg);
        errlog(trace);
        //phantom.exit();
    };

    page.onResourceError = function (resourceError) {
        errlog('Unable to load resource (#' + resourceError.id + 'URL:' + resourceError.url + ')');
        errlog('Error code: ' + resourceError.errorCode + '. Description: ' + resourceError.errorString);
        //phantom.exit();
    };

    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('failed to load the address');
        } else {
            t = Date.now() - t;
            console.log(page.content);
            //page.render('render.jpg', { format: 'jpeg', quality: '100' });
        }
        phantom.exit();
    });
}
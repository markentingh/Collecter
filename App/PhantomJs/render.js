var system = require('system'),
    fs = require('fs'),
    err = '';

function errlog(msg) {
    err += msg + '\n';
}

//follow redirects & include redirected URL in saved file
function renderPage(url) {
    var page = require('webpage').create();
    var redirectURL = null;

    page.settings.loadImages = false;
    //page.settings.userAgent = 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.120 Safari/537.36';
    //page.settings.webSecurityEnabled = false;
    //page.settings.resourceTimeout = 3000;
    //page.viewportSize = { width: 1920, height: 1080 };

    //setup resource handling
    page.onResourceRequested = function (request) {
        errlog('Request ' + JSON.stringify(request, undefined, 4));
    };

    //check for URL redirect
    page.onResourceReceived = function (resource) {
        if (url == resource.url && resource.redirectURL) {
            redirectURL = resource.redirectURL;
        }
    };

    //set up error handling
    page.onError = function (msg, trace) {
        errlog('Error: ' + msg);
        errlog(trace);
        //console.log(err);
        //phantom.exit(1);
    };

    page.onResourceError = function (resourceError) {
        errlog('Unable to load resource (#' + resourceError.id + 'URL:' + resourceError.url + ')');
        errlog('Error code: ' + resourceError.errorCode + '. Description: ' + resourceError.errorString);
    };

    page.open(url, function (status) {
        if (redirectURL) {
            //console.log(redirectURL + '\n');
            renderPage(redirectURL);
        } else if (status == 'success') {
            fs.write('file.html', url + '{\\!/}' + page.content, 'w');
            //page.render('render.jpg', { format: 'jpeg', quality: '100' });
            phantom.exit(1);
        } else {
            console.log('failed to load the address');
        }
    });
}

if (system.args.length === 1) {
    //not enough arguments
    console.log('Usage: render.js <some URL>');
    phantom.exit(1);
} else {
    //request page
    phantom.outputEncoding = "utf8";
    renderPage(system.args[1]);
    setTimeout(function () { phantom.exit(1) }, 1000 * 30);
}
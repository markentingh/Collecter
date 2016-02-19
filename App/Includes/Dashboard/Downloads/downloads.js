S.downloader = {
    list: [], urlCount: 0, cancelcheck: false, checkingservers: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,

    load: function () {
        $('#btnaddserver').off().on('click', S.downloader.buttons.showAddServer);
        $('#btndownload').off().on('click', S.downloader.buttons.showDownloadArticles);
        $('#btncanceldownloader, #btnstopdownloader').off().on('click', S.downloader.buttons.cancelDownloadArticles);
        $('#btnstartdownloader').off().on('click', S.downloader.buttons.startDownloads);
        $('#btnsaveaddserver').off().on('click', S.downloader.buttons.saveAddServer);
        $('#btncanceladdserver').off().on('click', S.downloader.buttons.hideAddServer);
        $('#lstservertype').off().on('change', S.downloader.buttons.changeServerType);
    },

    buttons: {
        showAddServer: function () {
            $('.form-addserver').show();
            $('#btnaddserver').hide();
            S.downloader.buttons.changeServerType();
        },

        hideAddServer: function () {
            $('.form-addserver').hide();
            $('#btnaddserver').show();
        },

        saveAddServer: function () {
            var serverType = $('#lstservertype').val();
            var options = {};
            switch (serverType) {
                case '0': //local
                    options = { type: serverType, title:'', settings:'localhost' };
                    break;
                case '1': //web server
                    var title = $('#txtaddservertitle').val();
                    var settings = $('#txtaddserveraddress').val();
                    options = { type: serverType, title: title, settings: settings };
                    break;
            }
            S.ajax.post('/api/Downloads/AddServer', options, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtaddservertitle').val('');
            $('#txtaddserveraddress').val('');
            S.downloader.buttons.hideAddServer();
        },

        showDownloadArticles(){
            $('.form-downloader').show();
            $('#btndownloader').hide();
        },

        cancelDownloadArticles() {
            S.downloader.checkingservers = false;
            S.downloader.cancelcheck = true;
            clearTimeout(S.downloader.timer);
            $('.form-downloader, .downloader-status').hide();
            $('.form-downloader .settings').show();
            $('.form-downloader .checking').hide();
            $('#btndownloader').show();
        },

        startDownloads: function () {
            S.downloader.checkingservers = true;
            S.downloader.urlCount = 0;
            $('.downloader-status .progress').css({ width: '0%' });
            $('.downloader-status').addClass('checking');
            $('.downloader-status .progress-msg')[0].innerHTML = "Distributing URLs to servers for downloading...";
            $('.form-downloader .settings').hide();
            $('.form-downloader .checking, .downloader-status').show();
            S.ajax.post('/api/Downloads/StartDownloads', {}, function () { S.ajax.callback.inject(arguments[0]); });
        },

        changeServerType: function () {
            var type = $('#lstservertype').val();
            $('.option-local, .option-webserver, .option-azure, .option-vpn').hide();
            $('').hide();
            switch (type) {
                case '0':
                    $('.option-local').show();
                    break;
                case '1':
                    $('.option-webserver').show();
                    break;
                case '2':
                    $('.option-azure').show();
                    break;
                case '3':
                    $('.option-vpn').show();
                    break
            }
        }
    },

    

    updateServerStatus: function (domain, total, index) {
        S.downloader.urlCount += total;
        $('.downloader-status .progress-msg')[0].innerHTML = "added " + total + " url(s) from " + domain + ".";
        $('.downloader-status .progress').css({width: Math.round((100 / S.downloader.list.length) * (index + 1)) + '%'});
    },

    finishAnalyzing: function (){
        $('.downloader-status .progress-msg')[0].innerHTML = S.downloader.urlCount + " total articles downloadd.";
        $('.downloader-status .progress').css({ width: '100%' });
        S.downloader.checkingservers = false;
    },

    checkedServer: function (domain) {
        $('.downloader-status .progress-msg')[0].innerHTML = S.downloader.urlCount + " total articles downloadd.";
        $('.downloader-status .progress').css({ width: '100%' });
    }
}

S.downloader.load();
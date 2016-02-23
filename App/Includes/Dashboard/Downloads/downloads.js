S.downloader = {
    list: [], totalDownloads: 0, downloading: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,

    load: function () {
        $('#btnaddserver').off().on('click', S.downloader.buttons.showAddServer);
        $('#btndownload').off().on('click', S.downloader.buttons.startDownloads);
        $('#btnstopdownloader').off().on('click', S.downloader.buttons.cancelDownloadArticles);
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
                    options = { type: serverType, title: '', settings: $('#txtaddlocaladdress').val() };
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

        cancelDownloadArticles() {
            S.downloader.downloading = false;
            clearTimeout(S.downloader.timer);
            $('.form-downloader, .downloader-status').hide();
            $('#btndownloader').show();
        },

        startDownloads: function () {
            $('.form-downloader').show();
            $('#btndownloader').hide();
            S.downloader.downloading = true;
            $('.form-downloader .checking-msg')[0].innerHTML = "Distributing URLs to servers for downloading...";
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

    loadServerFrames: function (index, serverId, host) {
        console.log('loadServerFrames');
        var container = $('.server' + index + ' .downloading');
        console.log(container);
        var url = 'http://' + host + '/Download';
        container[0].innerHTML = '<iframe frameborder="0" scrolling="no" src="' + url + '"></iframe>';
    },

    updateDownloadQueue: function (minus) {
        S.downloader.totalDownloads -= minus;
        $('.servers .total')[0].innerHTML = S.util.math.numberWithCommas(S.downloader.totalDownloads);
    }
}

S.downloader.load();
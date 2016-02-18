S.analyzer = {
    list: [], urlCount: 0, cancelcheck: false, checkingfeeds: false,
    countdown: 0, timer: null, timerStart: null, timerEnd: null, timerCurrent: null,

    load: function () {
        $('#btnaddserver').off().on('click', S.analyzer.buttons.showAddServer);
        $('#btnanalyze').off().on('click', S.analyzer.buttons.showAnalyzeArticles);
        $('#btncancelanalyzer, #btnstopanalyzer').off().on('click', S.analyzer.buttons.cancelAnalyzeArticles);
        $('#btnstartanalyzer').off().on('click', S.analyzer.buttons.startAnalyzer);
        $('#btnsaveaddserver').off().on('click', S.analyzer.buttons.saveAddServer);
        $('#btncanceladdserver').off().on('click', S.analyzer.buttons.hideAddServer);
        $('#lstservertype').off().on('change', S.analyzer.buttons.changeServerType);
    },

    buttons: {
        showAddServer: function () {
            $('.form-addserver').show();
            $('#btnaddserver').hide();
        },

        hideAddServer: function () {
            $('.form-addserver').hide();
            $('#btnaddserver').show();
        },

        saveAddServer: function () {
            var title = $('#txtaddservertitle').val();
            var address = $('#txtaddserveraddress').val();
            S.ajax.post('/api/Analyzer/AddServer', { title: title, address: address }, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtaddservertitle').val('');
            $('#txtaddserveraddress').val('');
            S.analyzer.buttons.hideAddServer();
        },

        showAnalyzeArticles(){
            $('.form-analyzer').show();
            $('#btnanalyzer').hide();
        },

        cancelAnalyzeArticles() {
            S.analyzer.checkingfeeds = false;
            S.analyzer.cancelcheck = true;
            clearTimeout(S.analyzer.timer);
            $('.form-analyzer, .analyzer-status').hide();
            $('.form-analyzer .settings').show();
            $('.form-analyzer .checking').hide();
            $('#btnanalyzer').show();
        },

        startAnalyzer: function () {
            S.analyzer.checkingfeeds = true;
            S.analyzer.urlCount = 0;
            $('.analyzer-status .progress').css({ width: '0%' });
            $('.analyzer-status').addClass('checking');
            $('.analyzer-status .progress-msg')[0].innerHTML = "Checking available analyzer for new links...";
            $('.form-analyzer .settings').hide();
            $('.form-analyzer .checking, .analyzer-status').show();
            S.ajax.post('/api/Analyzer/StartAnalyzer', {}, function () { S.ajax.callback.inject(arguments[0]); });
        },

        checkServer: function (index) {
            var more = false;
            if (S.analyzer.cancelcheck == true) {
                S.analyzer.cancelcheck = false;
                return;
            }
            if (S.analyzer.checkingfeeds == true) {
                more = true;
            } else {
                $('.analyzer-status .progress').css({ width: '0%' });
                $('.analyzer-status').addClass('checking');
                $('.analyzer-status .progress-msg')[0].innerHTML = "Checking selected feed for new links...";
                $('.analyzer-status').show();
            }
            S.ajax.post('/api/Analyzer/CheckServer', { index: index, more: more }, function () { S.ajax.callback.inject(arguments[0]); });
        },

        changeServerType: function () {
            var type = $('#lstfeedtype').val();
            $('.option-local, .option-azure, .option-vpn').hide();
            $('').hide();
            switch (type) {
                case '1':
                    $('.option-local').show();
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
        S.analyzer.urlCount += total;
        $('.analyzer-status .progress-msg')[0].innerHTML = "added " + total + " url(s) from " + domain + ".";
        $('.analyzer-status .progress').css({width: Math.round((100 / S.analyzer.list.length) * (index + 1)) + '%'});
    },

    finishAnalyzing: function (){
        $('.analyzer-status .progress-msg')[0].innerHTML = S.analyzer.urlCount + " total articles analyzed.";
        $('.analyzer-status .progress').css({ width: '100%' });
        S.analyzer.checkingfeeds = false;
    },

    checkedServer: function (domain) {
        $('.analyzer-status .progress-msg')[0].innerHTML = S.analyzer.urlCount + " total articles analyzed.";
        $('.analyzer-status .progress').css({ width: '100%' });
    }
}

S.analyzer.load();
S.article = {
    hub: null,

    init: function () {
        $('.analyze-article .analyze > .button').on('click', S.article.analyze.start);

        //set up signalR hub
        S.article.hub = new signalR.HubConnectionBuilder().withUrl('/articlehub').build();
        S.article.hub.on('update', S.article.analyze.update);
        S.article.hub.start().catch(S.article.analyze.error);
    },

    analyze: {
        start: function () {
            var url = S.util.location.queryString('url');
            if (url.indexOf('http') != 0 ||
                (url.indexOf('http://') != 0 && url.indexOf('https://') != 0)
            ) {
                S.article.message.error('Invalid URL');
                return;
            }
            $('.analyze-article .analyze').hide();
            $('.analyze-article > .box').css({ height: 400 });

            //send command via SignalR
            S.article.hub.invoke('AnalyzeArticle', url);
        },

        update: function (code, msg) {
            //receive command from SignalR
            var box = $('.analyze-article > .box')[0];
            var div = document.createElement('div');
            div.className = 'cli-line';
            div.innerHTML = '> ' + msg;
            $('.analyze-article .console').append(div);
            box.scrollTop = box.scrollHeight;
        },

        error: function (err) {

        }
    },

    message: {
        error: function (msg) {
            S.article.message.error(msg.toString());
        }
    }
};

S.article.init();
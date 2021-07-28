S.feeds = {
    load: function () {
        $('#form-addfeed').on('submit', function (e) {
            S.feeds.buttons.saveFeed(); e.preventDefault(); return false;
        });
        $('#btnaddfeed').off().on('click', S.feeds.add.show);
        $('#btnsavefeed').off().on('click', S.feeds.add.save);
        $('#btncanceladdfeed').off().on('click', S.feeds.add.hide);
    },

    add: {
        show: function () {
            $('.form-addfeed').show();
            $('#btnaddfeed').hide();
        },

        hide: function () {
            $('.form-addfeed').hide();
            $('#btnaddfeed').show();
        },

        save: function () {
            S.ajax.post('/Feeds/Add', { url: $('#txtfeed').val(), loadUI: true },
                function (data) {
                    var d = JSON.parse(data);
                    d.d.inject = 0; //replace
                    S.ajax.inject('.feed-list > .inner', d);
                });

            $('#txtfeed').val('');
            S.feeds.add.hide();
        }
    },

    hideAll: function () {
        $('.form-addfeed').hide();
    }
}
S.feeds.load();
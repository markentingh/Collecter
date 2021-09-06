S.feeds = {
    load: function () {
        $('#form-addfeed').on('submit', function (e) {
            S.feeds.buttons.saveFeed(); e.preventDefault(); return false;
        });
        $('#btnaddfeed').off().on('click', S.feeds.add.show);
        $('#btnsavefeed').off().on('click', S.feeds.add.save);
        $('#btncanceladdfeed').off().on('click', S.feeds.add.hide);
        $('#btnaddcategory').on('click', S.feeds.category.add.show);
        $('#btncreatecategory').on('click', S.feeds.category.add.submit);
        $('.form-addfeed').on('submit', S.feeds.add.submitform);
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

        submitform: function (e) {
            e.preventDefault();
        },

        save: function () {
            S.ajax.post('/Feeds/Add', {
                categoryId: $('#feedcategory').val(),
                url: $('#txtfeed').val(),
                title: $('#txtfeedtitle').val(),
            },
                function () {
                    location.reload();
                });

            $('#txtfeed').val('');
            S.feeds.add.hide();
        }
    },

    category: {
        add: {
            show: function () {
                $('.form-addfeed .categories').addClass('hide');
                $('.form-addfeed .new-category').removeClass('hide');
            },

            submit: function () {
                S.ajax.post('/Feeds/AddCategory', { title: $('#newcategorytitle').val() },
                    function (data) {
                        $('#feedcategory').html(data);
                    });
                S.feeds.category.add.cancel();
            },

            cancel: function () {
                $('.form-addfeed .categories').removeClass('hide');
                $('.form-addfeed .new-category').addClass('hide');
            }
        }
    },

    hideAll: function () {
        $('.form-addfeed').hide();
    }
}
S.feeds.load();
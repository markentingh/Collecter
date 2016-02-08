S.search = {
    load: function () {
        $('.form-search #search, .form-search .search-pretext').on('click, focus', S.search.buttons.focusSearchBox);
        $('.form-search #search').on('blur', S.search.buttons.blurSearchBox);
    },

    buttons: {
        focusSearchBox: function () {
            $('.search-pretext').hide();
            $('.form-search #search').focus();
        },

        blurSearchBox: function () {
            if ($('.form-search #search').val() == '') {
                $('.search-pretext').show();
            }
            
        }
    }
}

S.search.load();
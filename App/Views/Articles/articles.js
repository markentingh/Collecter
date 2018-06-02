S.articles = {
    init: function () {
        $('#btnaddarticle').on('click', S.articles.add.show);
    },

    add: {
        show: function () {
            S.popup.show("Add Article", $('#template_addarticle').html());
            $('.popup form').on('submit', S.articles.add.submit);
            $('.popup form .button.cancel').on('click', S.articles.add.hide);
        },

        hide: function () {
            S.popup.hide();
        },

        submit: function (e) {
            e.preventDefault();
            var url = $('#newarticle_url').val();
            if (url == '' || url == null) {
                alert('URL cannot be empty');
                return false;
            }
            window.location.href = "/article?url=" + encodeURIComponent(url);
            return false;
        }
    }
};

S.articles.init();
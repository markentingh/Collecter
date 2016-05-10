S.articles = {
    load: function () {
        $('#lstgroupby, #lstsortby, #lstviewby').off().on('change', S.articles.filterArticles);
        $('#btnarticlesearch').off().on('click', S.articles.filterArticles);
    },

    analyzeArticle: function(url) {
        $('#articleurl').val(url);
        $('#articlebtn')[0].click();
    },

    filterArticles: function() {
        var options = {
            element: '#articleslist',
            start: 1,
            length: 5,
            groupby: $('#lstgroupby').val(),
            sortby: $('#lstsortby').val(),
            viewby: $('#lstviewby').val(),
            feedId: -1,
            subjectId: 0,
            subjectIds: '',
            search: $('#articlesearch').val(),
            isActive: 2,
            isDeleted: false,
            minImages: 0,
            dateStart: '',
            dateEnd: ''
        };
        S.ajax.post('/api/Articles/GetArticlesUI', options, function () { S.ajax.callback.inject(arguments[0]); });

    },

    pagingArticles: function (start, feedId, subjectId) {
        var options = {
            element: (feedId >= 0 ? '.feed' + feedId + ' .contents' : (subjectId > 0 ? '.subject' + subjectId : '.feed00')),
            start: start,
            length: 100,
            groupby: $('#lstgroupby').val(),
            sortby: $('#lstsortby').val(),
            viewby: $('#lstviewby').val(),
            feedId: feedId,
            subjectId: subjectId,
            subjectIds: '',
            search: $('#articlesearch').val(),
            isActive: 2,
            isDeleted: false,
            minImages: 0,
            dateStart: '',
            dateEnd: ''
        };
        S.ajax.post('/api/Articles/GetArticlesUI', options, function () { S.ajax.callback.inject(arguments[0]); });

    }
}
S.articles.load();
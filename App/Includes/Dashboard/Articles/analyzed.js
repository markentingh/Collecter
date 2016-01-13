S.analyzed = {
    ace: null,

    load:function(){
        $('#btnsaveselectedwords').on('click', S.analyzed.buttons.saveSelectedWords);
        $('.words-sorted .box .word, .phrases .phrase').on('click', S.analyzed.buttons.toggleWord);
    },

    loadAce: function () {
        if (S.analyzed.ace == null) {
            S.analyzed.ace = ace.edit("rawhtml");
            S.analyzed.ace.setTheme("ace/theme/monokai");
            S.analyzed.ace.getSession().setMode("ace/mode/html");
            S.analyzed.ace.setReadOnly(true);
        }
    },

    buttons: {
        toggleWord: function(){
            $(this).toggleClass('expanded')
            var word = $(this).find(".value")[0].firstChild.nodeValue.toLowerCase();
            var words = $('#txtselectedwords').val().replace(/\,\s/g, ',').split(',');
            words = $.grep(words, function (value) { return value != word; });
            if ($(this).hasClass('expanded') == true) {
                if (words.length == 1) { if (words[0].length == 0) { words = [];}}
                words.push(word.toLowerCase());
            }
            $('#txtselectedwords').val(words.join(', '));
        },

        saveSelectedWords() {
            var type = $('#lstwordtype').val();
            var words = $('#txtselectedwords').val();
            var grammartype = $('#lstwordpart').val();
            var hierarchy = $('#txtwordcategory').val();
            var score = $('#txtwordscore').val();

            switch (type) {
                case "subject":
                    S.ajax.post('/api/Dashboard/Subjects/AddSubject', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('subject(s) added'); });
                    break;
                case "common":
                    S.ajax.post('/api/Dashboard/Articles/AddCommonWord', { word: words }, function () { alert('common word(s) added'); });
                    break;
                case "phrase":
                    S.ajax.post('/api/Dashboard/Articles/AddPhrase', { word: words }, function () { alert('phrase added'); });
                    break;
                case "word": S.ajax.post('/api/Dashboard/Articles/AddWords', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('word(s) added'); });
                    break;
            }
        }
    }
};

S.analyzed.load();
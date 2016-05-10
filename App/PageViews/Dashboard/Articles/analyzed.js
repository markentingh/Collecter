S.analyzed = {
    ace: null, articleId:0,

    load:function(){
        $('#btnsaveselectedwords').on('click', S.analyzed.buttons.saveSelectedWords);
        $('.words-sorted .box .word, .phrases .phrase').on('click', S.analyzed.buttons.toggleWord);
        $('.article .word').on('mouseenter', S.analyzed.buttons.onArticleWordHover);
        $('.article .word').on('mouseleave', S.analyzed.buttons.onArticleWordLeave);
        $('.article .word').on('click', S.analyzed.buttons.onArticleWordClick);
        $('#btnsubmitbug').on('click', S.analyzed.buttons.saveBugReport);
        S.analyzed.buttons.changeWordType(S.elem.get('lstwordtype'));
        $('.btn-add-training').on('click', S.analyzed.buttons.addTrainingSentence);
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
        timerWords: null,

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

        saveSelectedWords: function() {
            var type = $('#lstwordtype').val();
            var words = $('#txtselectedwords').val();
            var grammartype = $('#lstwordpart').val();
            var hierarchy = $('#txtwordcategory').val();
            var score = $('#txtwordscore').val();

            switch (type) {
                case "subject":
                    S.ajax.post('/api/Subjects/AddSubject', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score, loadUI: false, element: '' }, function () { alert('subject(s) added'); });
                    break;
                case "common":
                    S.ajax.post('/api/Articles/AddCommonWord', { word: words }, function () { alert('common word(s) added'); });
                    break;
                case "normal":
                    S.ajax.post('/api/Articles/AddNormalWord', { word: words }, function () { alert('normal word(s) added'); });
                    break;
                case "phrase":
                    S.ajax.post('/api/Articles/AddPhrase', { word: words }, function () { alert('phrase added'); });
                    break;
                case "word": S.ajax.post('/api/Articles/AddWords', { words: words, grammartype: grammartype, hierarchy: hierarchy, score: score }, function () { alert('word(s) added'); });
                    break;
            }
            $('#txtselectedwords').val('');
        },

        onArticleWordHover: function () {
            //select all words that are part of the same sentence
            clearTimeout(S.analyzed.buttons.timerWords);
            var elem = $(this);
            var names = elem.attr('class');
            var sentenceId = '';
            if (names != '' && names != null) {
                var classes = names.split(' ');
                var className = '';
                if (classes.length > 0) {
                    for (c in classes) {
                        className = classes[c];
                        if (className.indexOf('sentence') == 0) {
                            sentenceId = className;
                            break;
                        }
                    }
                }
                
            }
            $('.article .word').removeClass('selected');
            if (sentenceId != '') {
                $('.article .word.' + sentenceId).addClass('selected');
            }
        },

        onArticleWordLeave: function () {
            S.analyzed.buttons.timerWords = setTimeout(function () { $('.article .word').removeClass('selected'); }, 300);
        },

        onArticleWordClick: function () {
            S.popup.show($('.popup-sentence'));
            var classes = $(this)[0].className.split(' ');
            var sentence = '';
            //build sentence from word elements
            for (var x = 0; x < classes.length; x++) {
                if (classes[x].substring(0, 8) == 'sentence') {
                    //found sentence class
                    var words = $('.article .' + classes[x]);
                    for (var y = 0; y < words.length; y++) {
                        var add = true;
                        var word = $.trim(words[y].innerText);
                        if (y >= words.length - 3) {
                            switch (word.substring(word.length - 1)) {
                                case '.': case '?': case '!':
                                    word = word.substring(0, word.length - 1);
                                    break;
                            }
                        }
                        if (word == '') { add = false;}
                        if (add == true) {
                            sentence += word + (words[y].className.indexOf('space') >= 0 ? ' ' : ''); //optionally, add space after the word
                        }
                    }
                    $('.txt-sentence').val(sentence);
                    break;
                }
            }
            $('.txt-sentence').val()
        },

        changeWordType: function (elem) {
            var type = $(elem).val();
            $('.words-sorted .menu .option-field').hide();
            $('.words-sorted .menu .wordtype-' + type).show();
        },

        saveBugReport: function () {
            var title = $('#txtbugtitle').val();
            var description = $('#txtbugdescription').val();
            S.ajax.post('/api/Articles/AddBugReport', { articleId: S.analyzed.articleId, title: title, description: description }, function () { S.ajax.callback.inject(arguments[0]); });
            $('#txtbugtitle').val('');
            $('#txtbugdescription').val('');
        },

        addTrainingSentence: function () {
            S.ajax.post('/api/Neurons/AddTrainingSentence',
                {
                    sentence: $('.txt-sentence').val(),
                    valid: $('.test-type').val() == '1' ? true : false
                },
                function () {});
            S.popup.hide();
        }
    }
};

S.analyzed.load();
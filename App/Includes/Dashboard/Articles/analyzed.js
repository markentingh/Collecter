S.analyzed = {
    ace: null,

    load:function(){
        $('#btnsaveselectedwords').on('click', S.analyzed.buttons.saveSelectedWords);
        $('.words-sorted .box .word').on('click', S.analyzed.buttons.toggleWord);
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
            var word = $(this)[0].firstChild.nodeValue;
            var words = $('#txtselectedwords').val().replace(/\,\s/g, ',').split(',');
            var wr = $.grep(words, function (value) { return value != word; }); words = wr;
            if ($(this).hasClass('expanded') == true) {
                if (words.length == 1) { if (words[0].length == 0) { words = [];}}
                words.push(word.toLowerCase());
            }
            $('#txtselectedwords').val(words.join(', '));
        },

        saveSelectedWords() {
            console.log('save');
            var type = $('#lstwordtype').val();
            var words = $('#txtselectedwords').val();
            console.log(type);
            console.log(words);
            switch (type) {
                case "add":

                    break;
                case "common":
                    S.ajax.post('/api/Dashboard/Articles/AddCommonWord', { word: words }, function () { alert('word(s) added'); });
                    break;
            }
        }
    }
};

S.analyzed.load();
S.analyzed = {
    ace: null,

    load: function () {
        S.analyzed.ace.setReadOnly(true);
    },

    loadAce: function () {
        if (S.analyzed.ace == null) {
            S.analyzed.ace = ace.edit("rawhtml");
            S.analyzed.ace.setTheme("ace/theme/monokai");
            S.analyzed.ace.getSession().setMode("ace/mode/html");
        }
    }
};

S.analyzed.load();
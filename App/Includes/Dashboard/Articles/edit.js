
S.blocks = {
    saveChanges: function (id) {
        var val = editor.getValue();
        S.ajax.post('/api/Dashboard/Blocks/SaveChanges', { id: id, changes: val },
            function () {
                $('#savemsg').show().css({ opacity: 1 }).delay(5000).animate({ opacity: 0 }, 1000, function () { $(this).hide(); });
            });
    }
}
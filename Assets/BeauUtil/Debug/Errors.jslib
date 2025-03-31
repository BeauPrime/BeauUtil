var BeauUtilErrorLib = {
    BeauUtil_DisplayAssertionAlert: function (message) {
        var msg = UTF8ToString(message);
        window.alert("Assertion Failed!\n\n" + msg);
    }
};

mergeInto(LibraryManager.library, BeauUtilErrorLib);
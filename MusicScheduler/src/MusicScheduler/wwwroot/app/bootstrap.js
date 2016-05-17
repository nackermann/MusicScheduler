System.register(['angular2/platform/browser', './main'], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var browser_1, main_1;
    return {
        setters:[
            function (browser_1_1) {
                browser_1 = browser_1_1;
            },
            function (main_1_1) {
                main_1 = main_1_1;
            }],
        execute: function() {
            browser_1.bootstrap(main_1.App)
                .then(success => console.log(`Bootstrap success`))
                .catch(error => console.log(error));
        }
    }
});
//# sourceMappingURL=bootstrap.js.map
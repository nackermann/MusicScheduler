/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

var sass = require("gulp-sass");
var typescript = require('gulp-typescript');
var inlineNg2Template = require('gulp-inline-ng2-template');
var sourcemaps = require('gulp-sourcemaps');
var eventStream = require('event-stream');

var paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.sass = paths.webroot + "css/**/*.scss";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatSassDest = paths.webroot + "css/general.css";
paths.concatCssDest = paths.webroot + "css/site.min.css";

var config = {
    libBase: 'node_modules',
    lib: [
        require.resolve('bootstrap/dist/css/bootstrap.css'),
        require.resolve('systemjs/dist/system.src.js'),
        require.resolve('angular2/bundles/angular2.dev.js'),
        require.resolve('angular2/bundles/router.dev.js'),
        require.resolve('angular2/bundles/http.js'),
        require.resolve('jquery/dist/jquery.js'),
        require.resolve('bootstrap/dist/js/bootstrap.js')
    ]
};

gulp.task("clean:js",
    function(cb) {
        rimraf(paths.concatJsDest, cb);
    });

gulp.task("clean:css",
    function(cb) {
        rimraf(paths.concatCssDest, cb);
    });

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js",
    function() {
        return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
            .pipe(concat(paths.concatJsDest))
            .pipe(uglify())
            .pipe(gulp.dest("."));
    });

gulp.task("min:css",
    function() {
        return gulp.src([paths.css, "!" + paths.minCss])
            .pipe(concat(paths.concatCssDest))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });

gulp.task("sass",
    function() {
        return gulp.src(paths.sass)
            .pipe(concat(paths.concatSassDest))
            .pipe(sass().on("error", sass.logError))
            .pipe(gulp.dest("."));
    });

gulp.task('build.lib', function () {
    return gulp.src(config.lib, { base: config.libBase })
        .pipe(gulp.dest(paths.webroot + 'lib'));
});

gulp.task('build-prod', ['build.lib'], function () {
    var tsProject = typescript.createProject('./tsconfig.json', { typescript: require('typescript') });
    var tsSrcInlined = gulp.src([paths.webroot + '**/*.ts'], { base: paths.webroot })
        .pipe(inlineNg2Template({ base: paths.webroot }));
    return eventStream.merge(tsSrcInlined, gulp.src('Typings/**/*.ts'))
        .pipe(sourcemaps.init())
        .pipe(typescript(tsProject))
        .pipe(sourcemaps.write())
        .pipe(gulp.dest(paths.webroot));
});

gulp.task('build-dev', ['build.lib'], function () {

});

gulp.task("min", ["min:js", "sass", "min:css", "build-dev"]);
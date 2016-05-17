System.register(["angular2/core", "angular2/http", "rxjs/Rx", "rxjs/Observable"], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
        var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
        if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
        else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
        return c > 3 && r && Object.defineProperty(target, key, r), r;
    };
    var __metadata = (this && this.__metadata) || function (k, v) {
        if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
    };
    var core_1, http_1, http_2, Observable_1;
    var YoutubeFile, User, Info, App;
    return {
        setters:[
            function (core_1_1) {
                core_1 = core_1_1;
            },
            function (http_1_1) {
                http_1 = http_1_1;
                http_2 = http_1_1;
            },
            function (_1) {},
            function (Observable_1_1) {
                Observable_1 = Observable_1_1;
            }],
        execute: function() {
            class YoutubeFile {
            }
            exports_1("YoutubeFile", YoutubeFile);
            class User {
            }
            exports_1("User", User);
            class Info {
            }
            exports_1("Info", Info);
            let App = class App {
                constructor(_http) {
                    this._http = _http;
                    this.info = new Info();
                    this.ticks = 0;
                }
                ngOnInit() {
                    let timer = Observable_1.Observable.timer(1000, 5000);
                    timer.subscribe((t) => this.ticks = t);
                    timer.subscribe((t) => {
                        this.getInfo();
                    });
                }
                /**
                 * Gets the music info  from the server
                 */
                getInfo() {
                    this._http.get("api/info")
                        .map(this.parseResponse)
                        .catch(this.handleError)
                        .subscribe((info) => this.info = info);
                }
                /**
                 * Pauses or resumes the music
                 */
                pauseResume() {
                    this._http.post("api/pauseResume", "")
                        .map(this.parseResponse)
                        .catch(this.handleError)
                        .subscribe();
                }
                /**
                 * Pauses or resumes the music
                 */
                skip() {
                    this._http.post("api/skip", "")
                        .map(this.parseResponse)
                        .catch(this.handleError)
                        .subscribe();
                }
                /**
                 * Books the specified song
                 */
                bookSong(url, username) {
                    const headers = new http_1.Headers();
                    headers.append("Content-Type", "application/json");
                    this._http.post("api/bookSong", JSON.stringify({ "URL": url, "Name": username }), { headers: headers })
                        .map(this.parseResponse)
                        .catch(this.handleError)
                        .subscribe();
                }
                parseResponse(res) {
                    if (res.status < 200 || res.status >= 300) {
                        throw new Error(`Response status: ${res.status}`);
                    }
                    const body = res.json();
                    return body || {};
                }
                handleError(error) {
                    console.log(error);
                    return Observable_1.Observable.throw(error.message);
                }
            };
            App = __decorate([
                core_1.Component({
                    selector: "MusicSchedulerApp",
                    providers: [http_2.HTTP_PROVIDERS],
                    templateUrl: "./app/html/main.html"
                }), 
                __metadata('design:paramtypes', [http_1.Http])
            ], App);
            exports_1("App", App);
        }
    }
});
//# sourceMappingURL=main.js.map
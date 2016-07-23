export class YoutubeFile {
    name: string;
    url: string;
    path: string;
    downloaded: boolean;
    duration: number;
}

export class User {
    name: string;
    timePlayed: string;
    youtubeLinks: Array<YoutubeFile>;
}

export class Info {
    CurrentlyPlaying: string;
    Volume: string;
    IsPaused: boolean;
    Users: Array<User>;
}
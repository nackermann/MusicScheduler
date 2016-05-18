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
    currentlyPlaying: string;
    isPaused: boolean;
    users: Array<User>;
}
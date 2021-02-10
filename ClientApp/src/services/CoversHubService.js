import * as signalR from "@microsoft/signalr";

class CoversHubService {

    constructor() {
        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/CoversHub")
            .withAutomaticReconnect([500, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 15000, 20000, null])
            .configureLogging(signalR.LogLevel.Information)
            .build();
        hubConnection.start();
        this.connection = hubConnection;
    }

    registerAlbumUpdates(albumUpdates) {
        this.connection.on('AlbumUpdates', () => {
            albumUpdates();
        });
    }

    registerProcessing(processing) {
        this.connection.on('Processing', (text) => {
            processing(text);
        });
    }

    registerSpotifyTokenRefresh(tokenRefresh) {
        this.connection.on('SpotifyTokenRefresh', (token) => {
            tokenRefresh(token);
        });
    }
}

const CoversService = new CoversHubService();

export default CoversService;
// Get signalr.d.ts.ts from https://github.com/borisyankov/DefinitelyTyped (or delete the reference)
/// <reference path="signalr/signalr.d.ts" />
/// <reference path="jquery/jquery.d.ts" />

////////////////////
// available hubs //
////////////////////
//#region available hubs

interface SignalR {

    /**
      * The hub implemented by Web.Hubs.ResultatService
      */
    resultatService : ResultatService;
}
//#endregion available hubs

///////////////////////
// Service Contracts //
///////////////////////
//#region service contracts

//#region ResultatService hub

interface ResultatService {
    
    /**
      * This property lets you send messages to the ResultatService hub.
      */
    server : ResultatServiceServer;

    /**
      * The functions on this property should be replaced if you want to receive messages from the ResultatService hub.
      */
    client : any;
}

interface ResultatServiceServer {

    /** 
      * Sends a "send" message to the ResultatService hub.
      * Contract Documentation: ---
      * @param name {string} 
      * @param message {string} 
      * @return {JQueryPromise of void}
      */
    send(name : string, message : string) : JQueryPromise<void>;
}

//#endregion ResultatService hub

//#endregion service contracts



////////////////////
// Data Contracts //
////////////////////
//#region data contracts

//#endregion data contracts


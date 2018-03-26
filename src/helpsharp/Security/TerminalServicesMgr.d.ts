declare module server {
    const enum wtsConnectstateClass {
        wtsActive = 0,
        wtsConnected = 1,
        wtsConnectQuery = 2,
        wtsShadow = 3,
        wtsDisconnected = 4,
        wtsIdle = 5,
        wtsListen = 6,
        wtsReset = 7,
        wtsDown = 8,
        wtsInit = 9,
    }
    /** The terminal services mgr. */
    interface terminalServicesMgr {
    }
}
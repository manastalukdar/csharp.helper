declare module server {
	const enum formatMessageFlags {
		fORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100,
		fORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200,
		fORMAT_MESSAGE_FROM_SYSTEM = 0x00001000,
		fORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000,
		fORMAT_MESSAGE_FROM_HMODULE = 0x00000800,
		fORMAT_MESSAGE_FROM_STRING = 0x00000400,
	}
	const enum processState {
		started,
		stopped,
	}
	const enum showWindowStatus {
		sW_MINIMIZE = 6,
		sW_RESTORE = 9,
		sW_SHOW = 5,
	}
	interface miscellaneous {
	}
}

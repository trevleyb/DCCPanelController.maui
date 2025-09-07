namespace DCCCommon;

[Flags]
public enum SignalAspectEnum {
    Off = 0,         // 000000
    None = 0,        // 000000
    Red = 1 << 0,    // 000001
    Yellow = 1 << 2, // 000100
    Green = 1 << 4,  // 010000

    FlashRed = 1 << 1,    // 000010
    FlashYellow = 1 << 3, // 001000
    FlashGreen = 1 << 5,  // 100000

    RedYellow = Red | Yellow,                          // 000101
    FlashRedYellow = FlashRed | FlashYellow,           // 001010
    RedFlashYellow = Red | FlashYellow,                // 001001
    RedGreen = Red | Green,                            // 010001
    FlashRedGreen = FlashRed | Green,                  // 010010
    RedFlashGreen = Red | FlashGreen,                  // 100001
    YellowGreen = Yellow | Green,                      // 010100
    FlashYellowGreen = FlashYellow | Green,            // 011000
    YellowFlashGreen = Yellow | FlashGreen,            // 100100
    AllOn = Red | Yellow | Green,                      // 010101
    AllFlashing = FlashRed | FlashYellow | FlashGreen, // 101010
    AllOff = 0
}
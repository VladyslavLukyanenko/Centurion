package gg.centurion.yscheckouthelper

import org.bouncycastle.util.encoders.Hex
import java.util.*
import java.util.concurrent.ThreadLocalRandom

class RT {
  init {
    if (true) {
      // while__INVOKEDYNAMIC__();
      // fuck__INVOKEDYNAMIC__();
    }
    // super();
  }

  companion object {
    // while__INVOKEDYNAMIC__();
    // fuck__INVOKEDYNAMIC__();
    val byteHex: String
      get() {
        if (true) {
          // while__INVOKEDYNAMIC__();
          // fuck__INVOKEDYNAMIC__();
        }
        val byte0 = ByteArray(4)
        ThreadLocalRandom.current().nextBytes(byte0)
        return Hex.toHexString(byte0)
      }

    // while__INVOKEDYNAMIC__();
    // fuck__INVOKEDYNAMIC__();
    val rT: String
      get() {
        if (true) {
          // while__INVOKEDYNAMIC__();
          // fuck__INVOKEDYNAMIC__();
        }
        val uUID10000 = UUID.randomUUID()
        return "\"z=1&dm=yeezysupply.com&si=" + uUID10000 + "&ss=" + java.lang.Long.toString(
          System.currentTimeMillis(),
          36
        ) + "&sl=1&tt=" + Integer.toString(
          ThreadLocalRandom.current().nextInt(890, 5800), 36
        ) + "&bcn=%2F%2F" + byteHex + ".akstat.io%2F\""
      }
  }
}

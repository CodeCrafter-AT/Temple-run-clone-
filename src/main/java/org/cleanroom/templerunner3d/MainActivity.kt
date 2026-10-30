package org.cleanroom.templerunner3d

import android.app.Activity
import android.content.Context
import android.content.SharedPreferences
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager
import android.os.Bundle
import android.webkit.JavascriptInterface
import android.webkit.WebSettings
import android.webkit.WebView
import android.webkit.WebViewClient

class MainActivity : Activity(), SensorEventListener {

    private lateinit var webView: WebView
    private lateinit var sensorManager: SensorManager
    private var accelerometer: Sensor? = null
    private lateinit var sharedPreferences: SharedPreferences

    private var filteredTiltX = 0f
    private val alpha = 0.15f // Low-pass filter smoothing

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        sharedPreferences = getSharedPreferences("TempleRunnerPrefs", Context.MODE_PRIVATE)

        webView = WebView(this).apply {
            settings.apply {
                javaScriptEnabled = true
                domStorageEnabled = true
                databaseEnabled = true
                allowFileAccess = true
                loadsImagesAutomatically = true
                cacheMode = WebSettings.LOAD_NO_CACHE
            }
            webViewClient = WebViewClient()
            addJavascriptInterface(WebAppInterface(this@MainActivity), "AndroidBridge")
        }

        setContentView(webView)

        // Initialize accelerometer
        sensorManager = getSystemService(Context.SENSOR_SERVICE) as SensorManager
        accelerometer = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)

        // Load the 3D endless runner game assets
        webView.loadUrl("file:///android_asset/index.html")
    }

    override fun onResume() {
        super.onResume()
        accelerometer?.let {
            sensorManager.registerListener(this, it, SensorManager.SENSOR_DELAY_GAME)
        }
        webView.onResume()
        webView.evaluateJavascript("if (window.gameResume) window.gameResume();", null)
    }

    override fun onPause() {
        super.onPause()
        sensorManager.unregisterListener(this)
        webView.onPause()
        webView.evaluateJavascript("if (window.gamePause) window.gamePause();", null)
    }

    override fun onSensorChanged(event: SensorEvent?) {
        if (event == null || event.sensor.type != Sensor.TYPE_ACCELEROMETER) return

        // In portrait mode, event.values[0] is X-axis tilt (negative = tilted right, positive = tilted left)
        val rawX = -event.values[0] / 9.8f
        filteredTiltX = (alpha * rawX) + ((1.0f - alpha) * filteredTiltX)

        val clamped = filteredTiltX.coerceIn(-1.0f, 1.0f)
        webView.post {
            webView.evaluateJavascript("if (window.onSensorTilt) window.onSensorTilt($clamped);", null)
        }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {}

    inner class WebAppInterface(private val context: Context) {
        @JavascriptInterface
        fun getHighScore(): Int {
            return sharedPreferences.getInt("high_score", 0)
        }

        @JavascriptInterface
        fun saveHighScore(score: Int) {
            val currentHigh = sharedPreferences.getInt("high_score", 0)
            if (score > currentHigh) {
                sharedPreferences.edit().putInt("high_score", score).apply()
            }
        }

        @JavascriptInterface
        fun getTotalCoins(): Int {
            return sharedPreferences.getInt("total_coins", 0)
        }

        @JavascriptInterface
        fun addCoins(amount: Int) {
            val total = sharedPreferences.getInt("total_coins", 0) + amount
            sharedPreferences.edit().putInt("total_coins", total).apply()
        }
    }
}

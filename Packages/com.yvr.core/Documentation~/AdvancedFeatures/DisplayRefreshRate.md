# Display Refresh Rate

Display refresh rates control the times that a headset's screen refreshes per second and therefore affect the quality of the image displayed to users. A higher refresh rate enables better image quality. In general, the display refresh rate should be 75 Hz or higher, thereby making human eyes unlikely to feel screen flickering.


## Note

- Low display refresh rates may cause frame drops, display lag, screen tearing, latency, and more other problems, which hugely affects the app experience. The default display refresh rate for YVR apps is 72 Hz. Yon can set a higher refresh rate for your app if needed. For example, racing games usually require high display refresh rates to ensure screen smoothness.

- High display refresh rates may affect your app's performance. Therefore, if you want to set a high display refresh rate for your app, you must ensure that it is able to sustain that rate. In the debugging process, you can use relevant tools to monitor your app's performance at the high display refresh rate and make in-time adjustments if necessary.

- High display refresh rates may reduce the life of the device.


## Set Display Refresh Rate

1. Open your project in the Unity Editor.
2. From the top menu bar, select **Edit** > **Project Settings**.
3. In the Project Settings window, expand XR Plug-in Management, select **YVR** > **Android settings icon**.
4. Set Display Refresh Rates.

| **Refresh Rate** | **Description** |
| ---------------- | --------------- | 
| Default | Default refresh rate, which is 72 Hz. |
| Refresh Rate 72 | 72 Hz. |
| Refresh Rate 90 | 90 Hz (Recommended). |
| Refresh Rate 120 | 120 Hz. |
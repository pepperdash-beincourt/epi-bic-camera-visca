# VISCA

## License

Provided under MIT license

## Overview

This repo is for VISCA camera plugin.

## Device Specific

### Plugin Valid Communication methods

```c#
Comm
Tcpip
Udp
```

### RS-232 Communications

| Setting      | Value                       |
| ------------ | --------------------------- |
| Baud rate    | 9600 or 38,400 (selectable) |
| Data bits    | 8                           |
| Stop bits    | 1                           |
| Parity       | None                        |
| Flow control | none                        |

### VISCA-over-IP via TCP/IP Communications

| Setting      | Value |
| ------------ | ----- |
| Default IP   |       |
| Default Port | 5500  |
| Username     |       |
| Password     |       |

### VISCA-over-IP via UDP Communications

| Setting      | Value |
| ------------ | ----- |
| Default IP   |       |
| Default Port | 52381 |
| Username     |       |
| Password     |       |

#### Device Configuration

```json
{	
	"key": "camera-1",
	"name": "VISCA Camera",
	"type": "visca",
	"group": "pluginDevices",
	"properties": {
		"control": {},
		"pollTimeMs": 30000,
		"address": 1,
		"panSpeed": 12,
		"tiltSpeed": 10,
		"zoomSpeed": 3,
		"focusSpeed": 4,
		"privacyOnPreset": 15,
		"privacyOffPreset": 1,
		"presets": [
			{
				"id": 15,
				"name": "Privacy On"
			},
			{
				"id": 2,
				"name": "Preset 2"
			}
		]
	}
}
```

### Bridge Configuration

The following configuration is an example of the Bridge configuration.

```
{
	"key": "plugin-bridge-1",
	"uid": 11,
	"name": "Communication Bridge",
	"group": "api",
	"type": "eiscApi",
	"properties": {
		"control": {
		"tcpSshProperties": {
			"address": "127.0.0.2",
			"port": 0
		},
		"ipid": "1A"
		},
		"devices": [
			{
				"deviceKey": "camera-1",
				"joinStart": 1
			}
		]
	}
}
```

## SiMPL Bridge Joins

### Digitals
| dig-o                                     | I/O     | dig-i                 |
| ----------------------------------------- | ------- | --------------------- |
| Tilt Up                                   | 1       |                       |
| TiltDown                                  | 2       |                       |
| Pan Left                                  | 3       |                       |
| Pan Right                                 | 4       |                       |
| Zoom In                                   | 5       |                       |
| Zoom Out                                  | 6       |                       |
| Power On                                  | 7       | Power On Feedback     |
| Power Off                                 | 8       |                       |
|                                           | 9       | Is Online Feedback    |
| Home                                      | 10      |                       |
| Preset Select (Press)/Preset Store (Hold) | 11 - 26 |                       |
| Focus Near                                | 28      |                       |
| Focus Far                                 | 29      |                       |
|                                           | 30      | Preset Store Feedback |
| Preset Store (Press)                      | 31 - 46 |                       |
| Privacy On                                | 48      |                       |
| Privacy Off                               | 49      |                       |
| Trigger Auto Focus                        | 50      |                       |

## Analogs
| an_o                    | I/O | an_i                       |
| ----------------------- | --- | -------------------------- |
| Pan Speed               | 1   | Pan Speed Feedback         |
| Tilt Speed              | 2   | Tilt Speed Feedback        |
| Zoom Speed              | 3   | Zoom Speed Feedback        |
| Focus Speed             | 4   | Focus Speed Feedback       |
| Preset Select by Number | 11  | Number of Presets Feedback |
| Preset Store by Number  | 12  |                            |
|                         | 50  | Status                     |

## Serials
| serial-o           | I/O     | serial-i     |
| ------------------ | ------- | ------------ |
|                    | 1       | Device Name  |
|                    | 11 - 26 | Preset Names |
| Device Comms (WIP) | 50      |              |
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 1.11.1
<!-- END Minimum Essentials Framework Versions -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "ViscaCamera",
    "group": "Group",
    "properties": {
        "control": "SampleValue",
        "address": "SampleValue",
        "panSpeed": "SampleValue",
        "tiltSpeed": "SampleValue",
        "ZoomSpeed": "SampleValue",
        "FocusSpeed": "SampleValue",
        "PrivacyOnPreset": "SampleValue",
        "PrivacyOffPreset": "SampleValue",
        "pollTimeMs": 0,
        "presets": [
            {
                "name": "SampleString",
                "id": 0
            }
        ]
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->

<!-- END Supported Types -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Tilt Up |
| 2 | R | Tilt Down |
| 3 | R | Pan Left |
| 4 | R | Pan Right |
| 5 | R | Zoom In |
| 6 | R | Zoom Out |
| 7 | R | Camera power on |
| 8 | R | Camera power off |
| 9 | R | Is Online |
| 10 | R | Home |
| 11 | R | Preset select (press), store (hold) |
| 28 | R | Focus Near |
| 29 | R | FocusFar |
| 30 | R | Camera preset stored Feedback |
| 31 | R | Preset store |
| 48 | R | Privacy On |
| 49 | R | Privacy Off |
| 50 | R | Trigger auto focus |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Pan Speed |
| 2 | R | Tilt Speed |
| 3 | R | Zoom Speed |
| 4 | R | Focus Speed |
| 11 | R | Number of configured presets |
| 11 | R | Preset select by number |
| 12 | R | Preset store by number |
| 50 | R | Returns Socket Status when using VISCA-over-IP |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Name |
| 11 | R | Preset Name |
| 50 | R | Camera device communications |
<!-- END Join Maps -->
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- ICommunicationMonitor
- IRoutingSource
- IHasCameraOff
- IHasCameraPtzControl
- IHasCameraFocusControl
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- EssentialsBridgeableDevice
- JoinMapBaseAdvanced
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void SendBytes(byte[] bytes)
- public void SendCustomCommand(string cmd)
- public void InitializeCamera()
- public void Poll()
- public void CameraOn()
- public void CameraOff()
- public void PanLeft()
- public void PanRight()
- public void PanStop()
- public void TiltDown()
- public void TiltUp()
- public void TiltStop()
- public void ZoomIn()
- public void ZoomOut()
- public void ZoomStop()
- public void FocusNear()
- public void FocusFar()
- public void FocusStop()
- public void TriggerAutoFocus()
- public void PositionHome()
- public void PresetSelect(int preset)
- public void PresetStore(int preset, string description)
- public void PrivacyOn()
- public void PrivacyOff()
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- PresetStoredFeedback
- OnlineFeedback
- CameraIsOffFeedback
- AutoFocusFeedback
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- NumberOfPresetsFeedback
- SocketStatusFeedback
- MonitorStatusFeedback
- PanSpeedFeedback
- TiltSpeedFeedback
- ZoomSpeedFeedback
- FocusSpeedFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->

<!-- END String Feedbacks -->

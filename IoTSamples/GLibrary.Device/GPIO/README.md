# GLibrary.Device.GPIO Namespace

> General Purpose IO 

## GPIO Class

### Properties

#### PinNumber

Get PinNumber currently use.

```c#
public int PinNumber{ get; private set;}
```

#### DebounceTimeout

Get or set debounce timeout 

```c#
public TimeSpan DebounceTimeout{ get; set; }
```

### Contructors

#### GPIO(int PinNumber)

Create a new Gpio Pin with PinNumber

```c#
public GPIO(int PinNumber)
```

#### GPIO(int PinNumber, GpioSharingMode SharingMode)

Create a new Gpio Pin with PinNumber and Set GpioSharingMode

```c#
public GPIO(int PinNumber, GpioSharingMode SharingMode)
```

### Methords

#### SetDriveMode(GpioPinDriveMode PinDriveMode)

Set PinDriveMode, such as Input, Output, Input with pull-up and so on.

```c#
public void SetDriveMode(GpioPinDriveMode PinDriveMode)
```

#### GetDriveMode()

Get PinDriveMode value.

```c#
public GpioPinDriveMode GetDriveMode()
```

#### Read()

Read pin value .

```c#
public GpioPinValue Read()
```




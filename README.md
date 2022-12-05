# Booru Dataset Gatherer

<p align="center">
    A .NET Core 3.1 Console application to gather relevant information from Booru websites to create datasets. <br>
    <i>Powered by <a href="https://github.com/Xwilarg/BooruSharp" target="_blank">Boorusharp</a>, with support for Danbooru, E621, E926, Gelbooru and many more.</i>
</p>

<p align="center">
    <img src="https://i.imgur.com/P3b5LNk.png" style="width:40%;">
</p>

[![Publish](https://github.com/Corruptex/booru-dataset-getherer/actions/workflows/publish.yml/badge.svg)](https://github.com/Corruptex/booru-dataset-getherer/actions/workflows/publish.yml)

## Summary
<b>BooruDatasetGatherer</b> is an in .NET Core 3.1 Console application that aims to give the user an easy way to gather a large dataset from Booru based API's. With support for profiles, downloading images and gathering information inside a CSV dataset, it provides you with a set of tools to get you started with tagged Booru datasets for Machine Learning. The uses for this application could be for Object Recognition, Latent Diffusion, local Booru web-servers or other ends that require tagged images.

## Quick Start
Build the application using `dotnet build BooruDatasetGatherer` inside the cloned repository or use Visual Studio (Windows & Mac only) to compile a Debug or Release version.

## Arguments & Profiles
This application supports a range of different arguments has to be passed for it to function. The various arguments and settings will be explained below.

### Arguments
All arguments that are passed to the console application need to be prefixed with `--` and `strings` containing whitespace need to be wrapped in double quotes `"` for arguments to successfully parse. 

Example: `--source safebooru --filter "1girl, confetti" --nsfw false`

| Argument| Data Type | Description | Default | Required
| --- | --- | --- | --- | :---: |
| `source`  | `string`  | The Booru source from which to collect the dataset (`SafeBooru`, `Danbooru`, `DerpiBooru`) |  | :heavy_check_mark: |
| `filter`  | `string[]`| Comma separated tags, only posts with these tags will be collected (`eye_focus`, `nature`) |  | :heavy_multiplication_x: |
| `files`   | `string[]`| What type of file extensions can be accepted, recommended to filter out videos (`".png, .jpg"`, `.gif`) | `".png, .jpg, jpeg, .gif, .webp"` | :heavy_multiplication_x: |
| `download`| `bool`    | If enabled, images that are saved in the dataset will also be saved locally (`false`, `1`) | `false` | :heavy_multiplication_x: |
| `location`| `string`  | The location where the dataset (and if enabled images as well) will be saved to (`"C:\Dataset"`) | `{Executing Folder}/Images` | :heavy_multiplication_x: |
| `size`    | `int`     | The total size of the dataset (`1000`, `512`) | `1000` | :heavy_multiplication_x: |
| `batch`   | `int`     | The size that a batch is allowed to be. The total size is split among threads, the split size will be divided by the batch size in to cycles (`50`, `15`) | `20` | :heavy_multiplication_x: |
| `threads` | `byte`    | The amount of threads to split the dataset gathering task with (`4`, `16`) | `4` | :heavy_multiplication_x: |
| `nsfw`    | `bool`    | If enabled, images that contain Not Safe For Work (NSFW) content will be saved into the dataset and downloaded if `download` is enabled (`true`, `0`) | `false` | :heavy_multiplication_x: |
| `profile` | `string`  | The location (or name if located in the same folder) of the `.json` file containing a valid profile, more info in section [Profiles](#profiles) (`C:\booruProfile.json`, `booruProfile`) |    | :heavy_multiplication_x: |
| `username`| `string`  | The username of the Booru account that you wish to authenticate with (`"My Username"`, `my_username`) |   | :heavy_multiplication_x: |
| `passwordHash`| `string`  | The password used to authenticate yourself at the given source. Note: this can be your password hash, API key or a different key, depending on the Booru of your choosing. More info at [Authentication](#authentication) (`"{password_hash}"`, `{api_key}`). | `{Executing Folder}/Images` | :heavy_multiplication_x: |

In [Examples](#examples) multiple examples are given for different purposes using the above mentioned arguments.

### Profiles
To make it easier to repeat tasks, arguments can be contained inside a `.json` file. This is called a profile, with which you can easily load a certain set of arguments into the application without the need to pass any arguments yourself. Any argument that's passed with a `--profile` argument overrules the values inside the profile.

If in the `booruProfile` json the `size` argument is set to 20, it'll be overruled by the raw argument that's passed in. In this scenario
`--profile "booruProfile" --size 50`
the `size` will be 50, even though 20 is defined in the `booruProfile`.

A profile can look like this:
```json
{
    "source": "safebooru",
    "filter": [
        "eye_focus",
        "hair_between_eyes"
    ],
    "files": [
        ".png",
        ".jpeg"
    ],
    "download": true,
    "location": "C:/Dataset",
    "size": 50,
    "batch": 5,
    "threads": 5,
    "nsfw": false
}
```

## Examples
A set of examples to help you get on your way to using this tool. 
:warning: Caution: all examples only contain the parameters. The way the application is booted differs from platform to platform.

### Defaults
```powershell
--source safebooru
```

### Filters
```powershell
--source safebooru --filter "1girl, eye_focus"
```

### Negative filters
```powershell
--source safebooru --filter "-1girl, 1boy, -galaxy"
```

### Total- and batch-size
```powershell
--source safebooru --size 2000 --batch 50
```

### Pre-defined profile outside the executing directory
```powershell
--profile "C:/Profiles/safeBooruProfile.json"
```

### Pre-defined profile inside the executing directory
```powershell
---profile "safeBooruProfile"
```

### With authentication
```powershell
---source safebooru --username "{username}" --passwordHash "{password_hash}"
```

### Customizing all parameters
```powershell
--source safebooru --filter "1boy, confetti, celebrating" --size 1500 --batch 25 --threads 16 --download true --nsfw false --location "C:/Dataset" --files ".jpg, .jpeg"
```

## Supported Boorus
A large amount of the Boorus that <a href="https://github.com/Xwilarg/BooruSharp" target="_blank">Boorusharp</a> supports are available, though only the Boorus that support getting random images. Since this library uses the generic `ABooru` class of `BooruSharp`, only an abstract set of the functions are available. Because there is no abstract function to get posts using pagination, the functions for getting random images are supported. The complete list of supported boorus are as follows:

| Supported Boorus |
| --- |
| Atfbooru |
| Danbooru Donmai |
| Derpibooru |
| E621 |
| E926 |
| Gelbooru |
| Konachan |
| Lolibooru |
| Ponybooru |
| Realbooru |
| Sakugabooru |
| Sankaku Complex |
| Twibooru |
| Yandere |

The output of these Boorus are generic and will not differ, it can occur that a field will be empty. The dataset output will contain these values:
```
FILEURL, PREVIEWURL, POSTURL, SAMPLEURI, RATING, TAGS, ID, HEIGHT, WIDTH, PREVIEWHEIGHT, PREVIEWWIDTH, CREATION, SOURCE, SCORE, MD5, LOCATION
```

## Authentication
Since BooruSharp supports authentication, this application also supports this feature. By passing along a `--username` and `--passwordHash` you can authenticate yourself to the given Booru source. The different methods of authentication and which Booru supports it, you should consult the BooruSharp `README.md`, under the section [Authentication](https://github.com/Xwilarg/BooruSharp#authentification).

## Future
Since this application is made to gather information, a few things are on the road-map to further enhance that core feature. This includes:
* Support for custom Boorus
* Defining the fields that get exported to the dataset
* Support for Boorus that don't support random posts

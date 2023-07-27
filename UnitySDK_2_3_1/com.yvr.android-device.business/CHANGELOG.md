# ChangeLog

## [0.3.2] - 2023-05-26

### Fixed

- 修复 CallJINStatic方法返回值为空导致应用崩溃 的问题

## [0.3.1] - 2023-05-11

### Fixed

- 修复 ID 2 Method 未保存至字典中的问题

## [0.3.0] - 2023-03-30

### Added

- 在 AJCBase 中添加 CallJNIOverload 相关的 api，服务于调用的安卓函数存在重载

## [0.2.0] - 2023-03-17

- 建立 AJCFactory 和 Mocker 等机制，用以创建对于 Android Java Object 的封装
- 增加 CallJNI 相关接口，使用更底层的 JNI 接口直接调用函数，节省 Method2ID 开销。


# gitbook-integrator

Gitbook一键发布集成工具

### 功能

* Gitbook文档一键发布到[colin-chang.github.io](https://github.com/colin-chang/colin-chang.github.io)项目/books目录
* 合并Gitbook的Sitemap到[colin-chang.github.io](https://github.com/colin-chang/colin-chang.github.io)
* 此工具兼容 Linux/mac OSX

### 默认配置
* 一键发布多个Gitbook文档时，所有文档在同一目录下
![目录结构](dir.jpg)
* Gitbook使用此 [Sitemap插件](https://www.npmjs.com/package/gitbook-plugin-sitemap)
* 宿主网站使用 [Feeling Responsive](https://github.com/Phlow/feeling-responsive/) 模板

> 工具代码比较简单，不满足以上默认配置的朋友，根据需求自行[修改代码](https://github.com/colin-chang/gitbook-integrator/Program.cs)即可。

### 技术点
* 此工具基于 .net core 2.2 编写
* .net core 执行shell脚本和shell文件（含参数）
* .net core XML解析

### 运行

```sh
$ sudo sh integrator.sh
```

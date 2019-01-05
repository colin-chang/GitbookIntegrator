using System;
using System.IO;
using System.Linq;

namespace Integrator
{
    public static class PathUtil
    {
        /// <summary>
        /// 获得指定层级的父级路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string ParentPathUtil(this string path, int level)
        {
            var pp = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            return $"/{string.Join('/', pp.Take(pp.Length - level))}";
        }

        /// <summary>
        /// 获取文件名(包含无文件后缀的场景)
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public static string GetFileName(this string path)
        {
            return Path.HasExtension(path) ? Path.GetFileName(path) : path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        }
    }
}
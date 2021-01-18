using System;
using System.IO;

namespace BuildTypeDiscriminator
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("DLLファイルを１つ指定してください。");
				return (int)DiscriminationResult.ArgumentError;
			}

			string filePath = args[0];

			if (File.Exists(filePath) is false)
			{
				Console.WriteLine("指定されたパスにファイルが見つかりません。");
				return (int)DiscriminationResult.FileNotFoundError;
			}

			// 判別用の定数
			ReadOnlySpan<byte> PE = stackalloc byte[] { 0x50, 0x45 };
			ReadOnlySpan<byte> x64 = stackalloc byte[] { 0x00, 0x00, 0x64, 0x86 };
			ReadOnlySpan<byte> x86 = stackalloc byte[] { 0x00, 0x00, 0x4C, 0x01 };

			// ファイル読み込みバッファ
			Span<byte> peBuff = stackalloc byte[2];
			Span<byte> discriminateBuff = stackalloc byte[4];

			using var fs = new FileStream(filePath, FileMode.Open);
			int index = 0;

			// だいたい500以内には存在する
			while (index++ < 0x500)
			{
				fs.Read(peBuff);
				if (peBuff.SequenceEqual(PE))
				{
					fs.Read(discriminateBuff);
					break;
				}
			}

			if (discriminateBuff.SequenceEqual(x64))
			{
				Console.WriteLine("x64");
				return (int)DiscriminationResult.X64;
			}
			else if (discriminateBuff.SequenceEqual(x86))
			{
				Console.WriteLine("x86");
				return (int)DiscriminationResult.X86;
			}
			else
			{
				Console.WriteLine("想定外のファイルです。");
				return (int)DiscriminationResult.DiscriminationError;
			}
		}
	}

	internal enum DiscriminationResult
	{
		X64 = 0,
		X86 = 1,
		DiscriminationError = -1,
		ArgumentError = -2,
		FileNotFoundError = -3,
	}
}
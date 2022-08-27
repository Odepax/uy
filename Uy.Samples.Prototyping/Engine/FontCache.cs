using LinqToYourDoom;
using SharpGen.Runtime;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Vortice.DirectWrite;

sealed class FontCache : CallbackBase, IUnknown {
	readonly IDWriteFactory7 WriteFactory;
	readonly IDWriteInMemoryFontFileLoader FontLoader;
	readonly IDWriteFontSetBuilder2 FontSetBuilder;
	readonly List<(byte[] FontBytes, GCHandle FontBytesHandle)> FontData = new();

	public FontCache(IDWriteFactory7 writeFactory) {
		WriteFactory = writeFactory;
		FontLoader = WriteFactory.CreateInMemoryFontFileLoader();
		FontSetBuilder = WriteFactory.CreateFontSetBuilder();

		WriteFactory.RegisterFontFileLoader(FontLoader);
	}

	public FontCache Add(byte[] fontBytes) {
		FontData.Add((fontBytes, GCHandle.Alloc(fontBytes, GCHandleType.Pinned).Tee(out var fontBytesHandle)));

		using var file = FontLoader.CreateInMemoryFontFileReference(WriteFactory, fontBytesHandle.AddrOfPinnedObject(), fontBytes.Length, this);

		FontSetBuilder.AddFontFile(file);

		return this;
	}

	public IDWriteFontCollection1 BuildCollection() {
		using var fontSet = FontSetBuilder.CreateFontSet();

		return WriteFactory.CreateFontCollectionFromFontSet(fontSet);
	}

	protected override void DisposeCore(bool disposing) {
		if (!IsDisposed) {
			WriteFactory.UnregisterFontFileLoader(FontLoader);

			FontLoader.Dispose();
			FontSetBuilder.Dispose();

			foreach (var (_, fontBytesHandle) in FontData)
				fontBytesHandle.Free();
		}

		base.DisposeCore(disposing);
	}
}

using LinqToYourDoom;
using System;

namespace Uy;

sealed class Bug : BugException {
	public Bug(string bugId, Exception? innerException = null) : base(bugId, "https://github.com/Odepax/uy/issues"/*, innerException*/) {} // TODO: forward innerException (blocked on LTYD)
}

using LinqToYourDoom;

namespace Uy;

sealed class Bug : BugException {
	public Bug(string bugId) : base(bugId, "https://github.com/Odepax/uy/issues") {}
}

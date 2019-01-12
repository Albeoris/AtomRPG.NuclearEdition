#include "Unity.h"
#include "LoadMonoDynamically.h"

void EnableMonoDebug()
{
	const char *jitOptions = "--debugger-agent=transport=dt_socket,embedding=1,server=y,defer=y";
	char *jitArguments[1];
	jitArguments[0] = _strdup(jitOptions);
	mono_jit_parse_options(1, jitArguments);
	mono_debug_init(1);

	free(jitArguments[0]);
}

void UnityInit()
{
	SetupMono();
	EnableMonoDebug();
}
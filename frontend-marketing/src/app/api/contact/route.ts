export async function POST(request: Request) {
  const body = await request.json().catch(() => null);

  if (
    !body ||
    typeof body.name !== "string" ||
    !body.name.trim() ||
    typeof body.email !== "string" ||
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(body.email) ||
    typeof body.message !== "string" ||
    !body.message.trim()
  ) {
    return Response.json({ ok: false, error: "Invalid submission." }, { status: 400 });
  }

  return Response.json({ ok: true });
}

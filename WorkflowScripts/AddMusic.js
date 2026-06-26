import { program } from "commander"

async function getSongByUrl(ncsUrl) {
  const res = await fetch(ncsUrl)
  if (!res.ok) throw new Error(`Failed to fetch ${ncsUrl}: ${res.status}`)
  const html = await res.text()

  // Slug from canonical URL
  const canonicalMatch = html.match(/<link rel="canonical" href="https:\/\/ncs\.io\/([^"]+)"/)
  const slug = canonicalMatch?.[1] ?? new URL(ncsUrl).pathname.replace("/", "")

  // Title + artists from <h2>
  const h2Match = html.match(/<h2[^>]*>([\s\S]*?)<\/h2>/)
  const h2 = h2Match?.[1] ?? ""
  const titleMatch = h2.match(/^([\s\S]*?)<span/)
  const name = titleMatch?.[1].trim() ?? ""
  const artistMatches = [...h2.matchAll(/<a href="\/artist\/\d+\/([^"]+)">([^<]+)<\/a>/g)]
  const artists = artistMatches.map(m => ({
    name: m[2],
    url: `/artist/${m[1]}`
  }))

  // Genre from og:description
  const descMatch = html.match(/<meta name="description" content="[^-]+ - ([^,]+),/)
  const genre = descMatch?.[1].trim() ?? null

  // Cover from og:image
  const coverMatch = html.match(/<meta property="og:image" content="([^"]+)"/)
  const coverUrl = coverMatch?.[1] ?? null

  // Preview MP3 from waveform data-url
  const previewMatch = html.match(/data-url="([^"]+\.mp3)"/)
  const previewUrl = previewMatch?.[1] ?? null

  // Track ID from data-tid
  const tidMatch = html.match(/data-tid="([^"]+)"/)
  const id = tidMatch?.[1] ?? null

  // Mood tags from og:description (after genre)
  const tagsMatch = html.match(/<meta name="description" content="[^-]+ - [^,]+, ([^"]+)"/)
  const tags = tagsMatch?.[1]
    ? tagsMatch[1].split(", ").map(t => ({ name: t.trim() }))
    : []

  return {
    name,
    url: `/${slug}`,
    id,
    genre,
    artists,
    coverUrl,
    previewUrl,
    tags,
    download: id ? {
      regular: `https://ncs.io/track/download/${id}`,
      instrumental: `https://ncs.io/track/download/i_${id}`
    } : null
  }
}

program.argument("<url>")
program.parse()

console.log(JSON.stringify(await getSongByUrl(program.args[0])))
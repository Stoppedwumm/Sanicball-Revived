import ncs from 'nocopyrightsounds-api'
import { program } from "commander"

async function getSongByUrl(ncsUrl) {
  // Extract slug from URL: "https://ncs.io/dreamin" → "dreamin"
  const slug = new URL(ncsUrl).pathname.replace("/", "")

  // Search using the slug as the query
  const results = await ncs.search({ search: slug })

  // Find the exact match by comparing the song's url field
  const song = results.find(s => s.url === `/${slug}`)

  return song ?? null
}

program
    .argument("<url>")

program.parse()

console.log(JSON.stringify(await getSongByUrl(program.args[0])))
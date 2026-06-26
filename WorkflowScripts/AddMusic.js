import ncs from 'nocopyrightsounds-api'
import { program } from "commander"

async function getSongByUrl(ncsUrl) {
  const slug = new URL(ncsUrl).pathname.replace("/", "")

  // "Puzzle2024" → "Puzzle", "Dreamin2023" → "Dreamin"
  const searchTerm = slug.replace(/\d+$/, '').trim()

  const results = await ncs.search({ search: searchTerm })

  // Match by URL slug (case-insensitive)
  const song = results.find(s => s.url.toLowerCase() === `/${slug.toLowerCase()}`)

  return song ?? null
}

program
    .argument("<url>")

program.parse()

console.log(JSON.stringify(await getSongByUrl(program.args[0])))
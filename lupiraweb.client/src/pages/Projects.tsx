import Link from "../components/Link";

export default function Projects(): React.ReactElement {
  const projects = [
    {
      id: "example",
      name: "Example Project",
      description:
        "A sample project showcasing various technologies and features.",
      image: "https://via.placeholder.com/300x200?text=Project+Image",
    },
  ];

  return (
    <section className="space-y-6">
      <h2 className="text-2xl font-semibold">Projects</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {projects.map((project) => (
          <Link
            key={project.id}
            href={`/projects/${project.id}`}
            className="block"
          >
            <div className="bg-slate-700 rounded-lg overflow-hidden shadow-lg hover:shadow-xl transition-shadow">
              <img
                src={project.image}
                alt={project.name}
                className="w-full h-48 object-cover"
              />
              <div className="p-4">
                <h3 className="text-lg font-medium text-slate-200 mb-2">
                  {project.name}
                </h3>
                <p className="text-slate-400 text-sm">{project.description}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>

      <div className="pt-6 border-t border-slate-700">
        <a
          href="https://github.com/danbro96"
          className="text-teal-400 hover:text-teal-300"
        >
          View all on GitHub
        </a>
      </div>
    </section>
  );
}

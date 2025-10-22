using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper.Instance;

public abstract class MyBaseComponent(
    string name,
    string nickname,
    string description,
    string category,
    string subCategory)
    : GH_Component(name, nickname, description, category, subCategory);